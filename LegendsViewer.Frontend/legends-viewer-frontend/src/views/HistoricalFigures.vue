<script setup lang="ts">
import { useHistoricalFigureStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';
import HistoricalFigureFilter from '../components/filter/HistoricalFigureFilter.vue';

const store = useHistoricalFigureStore();
const icon = "mdi-account";
const title = "Historical Figures";
const subtitle = "Delve into the Lives of Those Who Changed the Course of History";
const overviewSubtitle = "Browse and search all historical figures";
const overviewTitle = "Overview";
const wikiKeyWord = "Historical_figure";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Caste', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders"
        :showFilters="true">
        <template v-slot:type-specific-filter="{ filters }">
            <HistoricalFigureFilter :title="title" :filters="filters"
                @update:filters="val => filters.splice(0, filters.length, ...val)" />
        </template>
    </WorldObjectsPage>
</template>