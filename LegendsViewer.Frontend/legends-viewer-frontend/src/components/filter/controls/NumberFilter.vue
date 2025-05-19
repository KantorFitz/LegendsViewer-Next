<template>
  <v-list-item>
    <v-checkbox
      style="float: left; margin: -10px;"
      :label="label"
      v-model="proxyActive"
    ></v-checkbox>
    <div style="float: right;">
      <v-row>
        <v-col>
          <v-select
            density="compact"
            label=""
            :items="operators"
            v-model="proxyOperator"
            width="120"
          ></v-select>
        </v-col>
        <v-col>
          <v-number-input
            density="compact"
            :reverse="false"
            controlVariant="stacked"
            label=""
            :hideInput="false"
            :inset="true"
            width="120"
            v-model="proxyValue"
          ></v-number-input>
        </v-col>
      </v-row>
    </div>
  </v-list-item>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  label: string;
  operators: string[];
  active: boolean;
  operator: string | null;
  value: number | null;
}>();
const emit = defineEmits(['update:active', 'update:operator', 'update:value']);

const proxyActive = computed({
  get: () => props.active,
  set: (val: boolean) => emit('update:active', val)
});
const proxyOperator = computed({
  get: () => props.operator,
  set: (val: string | null) => emit('update:operator', val)
});
const proxyValue = computed({
  get: () => props.value,
  set: (val: number | null) => emit('update:value', val)
});
</script>