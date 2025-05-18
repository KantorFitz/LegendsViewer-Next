<template>
        <v-list style="background-color: rgb(var(--v-theme-background));">
            <v-list-item>
                <div style="float: left; margin-top: 5px;">Alive</div>
                <v-btn-toggle style="float: right;" density="compact" v-model="localRules.Alive" divided>
                    <v-btn value="true" size="small">
                        <v-icon>mdi-check</v-icon>
                    </v-btn>
                    <v-btn value="false" size="small">
                        <v-icon>mdi-close</v-icon>
                    </v-btn>
                </v-btn-toggle>
            </v-list-item>
            <v-list-item>
                <div style="float: left; margin-top: 5px;">Deity</div>
                <v-btn-toggle style="float: right;" density="compact" v-model="localRules.Deity" divided>
                    <v-btn value="true" size="small">
                        <v-icon>mdi-check</v-icon>
                    </v-btn>
                    <v-btn value="false" size="small">
                        <v-icon>mdi-close</v-icon>
                    </v-btn>
                </v-btn-toggle>
            </v-list-item>
            <v-list-item>
                <div style="float: left; margin-top: 5px;">Special</div>
                <v-btn-toggle style="float: right;" density="compact" v-model="localRules.Special" divided>
                    <v-btn value="vampire" size="small">
                        <img :src="vampireImageData" width="24" height="24" />
                    </v-btn>
                    <v-btn value="werebeast" size="small">
                        <img :src="werebeastImageData" width="24" height="24" />
                    </v-btn>
                    <v-btn value="necromancer" size="small">
                        <img :src="necromancerImageData" width="24" height="24" />
                    </v-btn>
                </v-btn-toggle>
            </v-list-item>
        </v-list>
</template>

<script setup lang="ts">
import { ref, watch, watchEffect } from 'vue';
import { vampireImageData, werebeastImageData, necromancerImageData } from '../FamilyTree.vue';
import type { FilterRuleDto } from '../../stores/worldObjectStores'; // Adjust the path as needed

// Accept an array of filter rules as a prop
const props = defineProps<{
    title: string
    filters: FilterRuleDto[] | null;
}>();

// Create local copies for binding. You might use a more structured model instead.
const localRules = ref({
    Alive: '',      // e.g., "true" or "false"
    Deity: '',      // e.g., "true" or "false"
    Special: '',    // e.g., "vampire", "werebeast", or "necromancer"
});

// Whenever filters change (or on mount), update localRules
watchEffect(() => {
    const filters = props.filters ?? [];

    const findRuleValue = (prop: string) =>
        filters.find(r => r.propertyName === prop && r.operator === "Equals")?.value ?? '';

    localRules.value.Alive = findRuleValue("IsAlive");
    localRules.value.Deity = findRuleValue("IsDeity");

    if (findRuleValue("IsVampire") === "true") {
        localRules.value.Special = "vampire";
    } else if (findRuleValue("IsWerebeast") === "true") {
        localRules.value.Special = "werebeast";
    } else if (findRuleValue("IsNecromancer") === "true") {
        localRules.value.Special = "necromancer";
    } else {
        localRules.value.Special = '';
    }
});

watch(localRules, () => {
    if (!props.filters) return;

    // Helper: Remove any rules by property name
    const removeRule = (propName: string) => {
        const index = props.filters!.findIndex(r => r.propertyName === propName);
        if (index !== -1) props.filters!.splice(index, 1);
    };

    const setRule = (propName: string, value: string) => {
        const existing = props.filters!.find(r => r.propertyName === propName);
        if (existing) {
            existing.value = value;
        } else {
            props.filters!.push({
                propertyName: propName,
                operator: "Equals",
                value: value
            });
        }
    };

    // ALIVE filter logic
    if (localRules.value.Alive === '') {
        removeRule("IsAlive");
    } else {
        setRule("IsAlive", localRules.value.Alive);
    }

    // DEITY filter logic
    if (localRules.value.Deity === '') {
        removeRule("IsDeity");
    } else {
        setRule("IsDeity", localRules.value.Deity);
    }

    // SPECIAL filter logic
    const specials = ["IsVampire", "IsWerebeast", "IsNecromancer"];
    specials.forEach(removeRule);

    switch (localRules.value.Special) {
        case 'vampire':
            setRule("IsVampire", "true");
            break;
        case 'werebeast':
            setRule("IsWerebeast", "true");
            break;
        case 'necromancer':
            setRule("IsNecromancer", "true");
            break;
    }
}, { deep: true });

</script>