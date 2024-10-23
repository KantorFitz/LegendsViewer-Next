﻿namespace LegendsViewer.Backend.Legends.Maps;

using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using SkiaSharp;

public class WorldMapImageGenerator(IWorld worldDataService) : IWorldMapImageGenerator
{
    public const int DefaultTileSizeMin = 2;
    public const int DefaultTileSizeMid = 4;
    public const int DefaultTileSizeMax = 10;
    private const int ThicklineInterval = 16;
    private const int ThinlineInterval = 4;
    private readonly IWorld _worldDataService = worldDataService;
    private byte[]? _worldMapMin;
    private byte[]? _worldMapMid;
    private byte[]? _worldMapMax;

    private SKBitmap? _exportedWorldMapBitmap;

    public async Task LoadExportedWorldMapAsync(string? bmpFilePath)
    {
        if (string.IsNullOrEmpty(bmpFilePath) || !File.Exists(bmpFilePath))
        {
            return;
        }

        using (var fileStream = new FileStream(bmpFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        using (var memoryStream = new MemoryStream())
        {
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the position of the memory stream to the beginning
            _exportedWorldMapBitmap = SKBitmap.Decode(memoryStream);
        }
    }

    public byte[]? GenerateMapByteArray(int tileSize = DefaultTileSizeMid, int? depth = null, IHasCoordinates? objectWithCoordinates = null)
    {
        if (objectWithCoordinates == null && depth == null && TryGetCachedMap(tileSize, out byte[]? imageData))
        {
            return imageData;
        }

        int pixelWidth = _worldDataService.Width * tileSize;
        int pixelHeight = _worldDataService.Height * tileSize;

        SKBitmap worldImage = new(pixelWidth, pixelHeight);
        if (_exportedWorldMapBitmap != null)
        {
            using (var canvas = new SKCanvas(worldImage))
            {
                // Draw the resized BMP as the base image
                canvas.DrawBitmap(_exportedWorldMapBitmap, new SKRect(0, 0, pixelWidth, pixelHeight));
            }
        }
        else
        {
            using (var canvas = new SKCanvas(worldImage))
            {
                DrawGrid(canvas, _worldDataService.Width, _worldDataService.Height, tileSize);
            }
        }

        worldImage = GenerateMapImage(worldImage, tileSize, objectWithCoordinates, depth);

        using (var stream = new SKDynamicMemoryWStream())
        {
            worldImage.Encode(stream, SKEncodedImageFormat.Png, 100);
            imageData = stream.DetachAsData().ToArray();
            if (imageData != null)
            {
                if (objectWithCoordinates == null && depth == null)
                {
                    switch (tileSize)
                    {
                        case DefaultTileSizeMin:
                            _worldMapMin = imageData;
                            break;
                        case DefaultTileSizeMid:
                            _worldMapMid = imageData;
                            break;
                        case DefaultTileSizeMax:
                            _worldMapMax = imageData;
                            break;
                    }
                }
                return imageData;
            }
        }
        return null;
    }

    public void Clear()
    {
        _worldMapMin = null;
        _worldMapMid = null;
        _worldMapMax = null;
        _exportedWorldMapBitmap = null;
    }

    private SKBitmap GenerateMapImage(SKBitmap worldImage, int tileSize, IHasCoordinates? objectWithCoordinates = null, int? depth = null)
    {
        IRegion[,] worldTiles = GetWorldTiles(depth);

        int width = worldTiles.GetLength(0);
        int height = worldTiles.GetLength(1);

        // Create a hash set of the object's coordinates for faster lookup
        HashSet<(int, int)> objectCoordinates = new();
        if (objectWithCoordinates != null)
        {
            foreach (var coord in objectWithCoordinates.Coordinates)
            {
                objectCoordinates.Add((coord.X, coord.Y));  // Assuming Location has X, Y properties
            }
        }

        using (var canvas = new SKCanvas(worldImage))
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SKColor tileColor;

                    // Check if this tile belongs to the objectWithCoordinates
                    if (objectCoordinates.Count > 0 && objectCoordinates.Contains((x, y)))
                    {
                        // Color the tile magenta if it belongs to the object
                        tileColor = SKColors.Magenta;
                    }
                    else
                    {
                        // Otherwise, get the region type and color
                        RegionType regionType = worldTiles[x, y]?.RegionType ?? RegionType.Default;
                        tileColor = GetRegionColor(regionType, worldTiles[x, y]?.Id);
                    }

                    // Draw a rectangle representing the tile
                    using (var paint = new SKPaint { Color = tileColor })
                    {
                        canvas.DrawRect(x * tileSize, y * tileSize, tileSize, tileSize, paint);
                    }
                }
            }

            // Then, draw a circle around the object for better visibility
            if (objectWithCoordinates != null && objectWithCoordinates.Coordinates.Count > 0)
            {
                EncircleObject(tileSize, objectWithCoordinates, canvas);
            }

            return worldImage;
        }
    }

    // Method to get a region color with variation based on the region's Id
    public static SKColor GetRegionColor(RegionType regionType, int? regionId)
    {
        SKColor baseColor = RegionTypeColors.BaseRegionColors[regionType];
        if (regionId == null)
        {
            return baseColor;
        }
        // Apply a color variation based on the region Id
        return ApplyIdBasedVariation(baseColor, regionId.Value);
    }

    // Apply a slight color variation based on region Id
    private static SKColor ApplyIdBasedVariation(SKColor baseColor, int regionId)
    {
        // Generate a seed based on the regionId to introduce a consistent variation
        Random random = new(regionId);

        // Slightly adjust each color component using the seed
        byte r = (byte)Math.Clamp(baseColor.Red + random.Next(-15, 15), 0, 255);
        byte g = (byte)Math.Clamp(baseColor.Green + random.Next(-15, 15), 0, 255);
        byte b = (byte)Math.Clamp(baseColor.Blue + random.Next(-15, 15), 0, 255);

        return new SKColor(r, g, b);
    }

    private static void EncircleObject(int tileSize, IHasCoordinates objectWithCoordinates, SKCanvas canvas)
    {
        // Calculate the center of the object
        var centerX = objectWithCoordinates.CenterX();
        var centerY = objectWithCoordinates.CenterY();

        // Convert the center to pixel coordinates
        int strokeThickness = tileSize / 2;
        float pixelCenterX = (float)(centerX * tileSize + strokeThickness);
        float pixelCenterY = (float)(centerY * tileSize + strokeThickness);

        // Convert the width and height to pixel coordinates
        float pixelWidth = objectWithCoordinates.Width() * tileSize;
        float pixelHeight = objectWithCoordinates.Height() * tileSize;

        // Define the circle size and outline
        float pixelRadiusX = (float)pixelWidth / 2 + (tileSize * 2);
        float pixelRadiusY = (float)pixelHeight / 2 + (tileSize * 2);
        using (var circlePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Yellow,
            StrokeWidth = strokeThickness
        })
        {
            canvas.DrawOval(pixelCenterX, pixelCenterY, pixelRadiusX, pixelRadiusY, circlePaint);
        }
        using (var circlePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.IndianRed,
            StrokeWidth = strokeThickness
        })
        {
            canvas.DrawOval(pixelCenterX, pixelCenterY, pixelRadiusX + strokeThickness, pixelRadiusY + strokeThickness, circlePaint);
        }
    }

    private bool TryGetCachedMap(int tileSize, out byte[]? imageData)
    {
        switch (tileSize)
        {
            case DefaultTileSizeMin:
                if (_worldMapMin != null)
                {
                    imageData = _worldMapMin;
                    return true;
                }
                break;
            case DefaultTileSizeMid:
                if (_worldMapMid != null)
                {
                    imageData = _worldMapMid;
                    return true;
                }
                break;
            case DefaultTileSizeMax:
                if (_worldMapMax != null)
                {
                    imageData = _worldMapMax;
                    return true;
                }
                break;
        }
        imageData = null;
        return false;
    }

    private IRegion[,] GetWorldTiles(int? depth = null)
    {
        IRegion[,] worldTiles = new IRegion[_worldDataService.Width + 1, _worldDataService.Height + 1];

        if (depth == null)
        {
            GetTilesBasedOnDepth(worldTiles, _worldDataService.Regions.OfType<IRegion>());
        }
        else
        {
            GetTilesBasedOnDepth(worldTiles, _worldDataService.UndergroundRegions.OfType<IRegion>(), depth);
        }

        return worldTiles;
    }

    private static void GetTilesBasedOnDepth(IRegion[,] worldTiles, IEnumerable<IRegion> regions, int? depth = null)
    {
        foreach (var region in regions.Where(r => r.Depth == depth))
        {
            foreach (var location in region.Coordinates)
            {
                worldTiles[location.X, location.Y] = region;
            }
        }
    }

    private void DrawGrid(SKCanvas canvas, int width, int height, int tileSize)
    {
        using (var thinPaint = new SKPaint { Color = SKColors.DarkSlateGray, StrokeWidth = 0 })
        using (var thickPaint = new SKPaint { Color = SKColors.DarkSlateBlue, StrokeWidth = 0 })
        {
            for (int x = tileSize; x <= (width * tileSize) - tileSize; x += tileSize)
            {
                if ((x / tileSize) % ThicklineInterval == 0)
                {
                    canvas.DrawLine(x, 0, x, height * tileSize, thickPaint);
                }
                else if ((x / tileSize) % ThinlineInterval == 0)
                {
                    canvas.DrawLine(x, 0, x, height * tileSize, thinPaint);
                }
            }

            for (int y = tileSize; y <= (height * tileSize) - tileSize; y += tileSize)
            {
                if ((y / tileSize) % ThicklineInterval == 0)
                {
                    canvas.DrawLine(0, y, width * tileSize, y, thickPaint);
                }
                else if ((y / tileSize) % ThinlineInterval == 0)
                {
                    canvas.DrawLine(0, y, width * tileSize, y, thinPaint);
                }
            }
        }
    }
}
