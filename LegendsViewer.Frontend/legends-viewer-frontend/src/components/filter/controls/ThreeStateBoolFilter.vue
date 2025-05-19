<template>
  <v-list-item>
    <div style="float: left; margin-top: 5px;">{{ label }}</div>
    <v-btn-toggle
      style="float: right;"
      density="compact"
      v-model="proxyValue"
      divided
    >
      <v-btn value="true" size="small"><v-icon>mdi-check</v-icon></v-btn>
      <v-btn value="false" size="small"><v-icon>mdi-close</v-icon></v-btn>
    </v-btn-toggle>
  </v-list-item>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{ label: string; modelValue: string }>();
const emit = defineEmits(['update:modelValue']);

const proxyValue = computed({
  get: () => props.modelValue,
  set: (val: string) => emit('update:modelValue', val)
});
</script>