import { defineStore } from 'pinia';
import client from '../apiClient';
import { components } from '../generated/api-schema';

export type CreatureInfo = components['schemas']['CreatureInfo'];
export type CreatureInfoPaginatedResponse = components['schemas']['CreatureInfoPaginatedResponse'];

export const useCreatureInfoStore = defineStore('creatureInfo', {
  state: () => ({
    items: [] as CreatureInfo[],
    totalCount: 0 as number,
    totalFilteredCount: 0 as number,
    pageSize: 20 as number,
    pageNumber: 1 as number,
    totalPages: 0 as number,
    loading: false as boolean,
    search: '' as string,
    error: '' as string,
  }),
  actions: {
    async loadCreatureInfo(pageNumber: number = 1, pageSize: number = 20, search: string = '') {
      this.loading = true;
      this.error = '';
      try {
        const { data, error } = await client.GET('/api/CreatureInfo', {
          params: { query: { pageNumber, pageSize, search } }
        });
        if (error) {
          this.error = typeof error === 'string' ? error : (error.message ? String(error.message) : 'Failed to load creature info.');
        } else if (data) {
          const response = data as CreatureInfoPaginatedResponse;
          this.items = response.items ?? [];
          this.totalCount = response.totalCount ?? 0;
          this.totalFilteredCount = response.totalFilteredCount ?? 0;
          this.pageSize = response.pageSize ?? pageSize;
          this.pageNumber = response.pageNumber ?? pageNumber;
          this.totalPages = response.totalPages ?? 0;
        }
      } catch (err: any) {
        this.error = err && err.message ? String(err.message) : 'Failed to load creature info.';
      } finally {
        this.loading = false;
      }
    },
    setSearch(search: string) {
      this.search = search;
    },
    setPage(pageNumber: number) {
      this.pageNumber = pageNumber;
    },
    setPageSize(pageSize: number) {
      this.pageSize = pageSize;
    }
  },
});
