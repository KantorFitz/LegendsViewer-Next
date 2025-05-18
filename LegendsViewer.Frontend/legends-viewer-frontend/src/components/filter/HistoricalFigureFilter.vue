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
        <v-list-item>
            <div style="float: left; margin-top: 5px;">Age</div>
            <div style="float: right;">
                <v-row>
                    <v-col>
                        <v-select density="compact" label="" :items="['Equals','NotEquals', 'GreaterThan', 'LessThan']" v-model="localRules.AgeOperator" width="120"></v-select>
                    </v-col>
                    <v-col>
                        <v-number-input density="compact" :reverse="false" controlVariant="stacked" label="" :hideInput="false"
                            :inset="true" width="120" v-model="localRules.AgeValue"></v-number-input>
                    </v-col>
                </v-row>
            </div>
        </v-list-item>
        <v-list-item>
            <div style="float: left; margin-top: 5px;">Born</div>
            <div style="float: right;">
                <v-row>
                    <v-col>
                        <v-select density="compact" label="" :items="['Equals','NotEquals', 'GreaterThan', 'LessThan']" v-model="localRules.BornOperator" width="120"></v-select>
                    </v-col>
                    <v-col>
                        <v-number-input density="compact" :reverse="false" controlVariant="stacked" label="" :hideInput="false"
                            :inset="true" width="120" v-model="localRules.BornValue"></v-number-input>
                    </v-col>
                </v-row>
            </div>
        </v-list-item>
        <v-list-item>
            <div style="float: left; margin-top: 5px;">Died</div>
            <div style="float: right;">
                <v-row>
                    <v-col>
                        <v-select density="compact" label="" :items="['Equals','NotEquals', 'GreaterThan', 'LessThan']" v-model="localRules.DiedOperator" width="120"></v-select>
                    </v-col>
                    <v-col>
                        <v-number-input density="compact" :reverse="false" controlVariant="stacked" label="" :hideInput="false"
                            :inset="true" width="120" v-model="localRules.DiedValue"></v-number-input>
                    </v-col>
                </v-row>
            </div>
        </v-list-item>
    </v-list>
</template>

<script setup lang="ts">
import { ref, watch, watchEffect } from 'vue';
import { vampireImageData, werebeastImageData, necromancerImageData } from '../FamilyTree.vue';
import type { FilterOperator, FilterRuleDto } from '../../stores/worldObjectStores'; // Adjust the path as needed

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
    AgeOperator: 'Equals' as FilterOperator,
    AgeValue: 0,
    BornOperator: 'Equals' as FilterOperator,
    BornValue: 0,
    DiedOperator: 'Equals' as FilterOperator,
    DiedValue: 0
});

// Whenever filters change (or on mount), update localRules
watchEffect(() => {
    const filters = props.filters ?? [];

    const findRuleValue = (prop: string) =>
        filters.find(r => r.propertyName === prop)?.value ?? '';

    const findRuleNumberValue = (prop: string) =>
        {
            var stringValue = filters.find(r => r.propertyName === prop)?.value;
            return stringValue ? parseInt(stringValue) : 0;
        };

    const findRuleOperator = (prop: string) =>
        filters.find(r => r.propertyName === prop)?.operator ?? 'Equals';

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
    localRules.value.AgeOperator = findRuleOperator("Age");
    localRules.value.AgeValue = findRuleNumberValue("Age");
    localRules.value.BornOperator = findRuleOperator("BirthYear");
    localRules.value.BornValue = findRuleNumberValue("BirthYear");
    localRules.value.DiedOperator = findRuleOperator("DeathYear");
    localRules.value.DiedValue = findRuleNumberValue("DeathYear");
});

watch(localRules, () => {
    if (!props.filters) return;

    // Helper: Remove any rules by property name
    const removeRule = (propName: string) => {
        const index = props.filters!.findIndex(r => r.propertyName === propName);
        if (index !== -1) props.filters!.splice(index, 1);
    };

    const setRule = (propName: string, operator: FilterOperator, value: string) => {
        const existing = props.filters!.find(r => r.propertyName === propName);
        if (existing) {
            existing.value = value;
        } else {
            props.filters!.push({
                propertyName: propName,
                operator: operator,
                value: value
            });
        }
    };

    // ALIVE filter logic
    if (localRules.value.Alive === '') {
        removeRule("IsAlive");
    } else {
        setRule("IsAlive", "Equals", localRules.value.Alive);
    }

    // DEITY filter logic
    if (localRules.value.Deity === '') {
        removeRule("IsDeity");
    } else {
        setRule("IsDeity", "Equals", localRules.value.Deity);
    }

    // AGE filter logic
    if (localRules.value.AgeValue == null || localRules.value.AgeValue === 0) {
        removeRule("Age");
    } else {
        setRule("Age", localRules.value.AgeOperator, localRules.value.AgeValue.toString());
    }

    // BORN filter logic
    if (localRules.value.BornValue == null || localRules.value.BornValue === 0) {
        removeRule("BirthYear");
    } else {
        setRule("BirthYear", localRules.value.BornOperator, localRules.value.BornValue.toString());
    }

    // DIED filter logic
    if (localRules.value.DiedValue == null || localRules.value.DiedValue === 0) {
        removeRule("DeathYear");
    } else {
        setRule("DeathYear", localRules.value.DiedOperator, localRules.value.DiedValue.toString());
    }

    // SPECIAL filter logic
    const specials = ["IsVampire", "IsWerebeast", "IsNecromancer"];
    specials.forEach(removeRule);

    switch (localRules.value.Special) {
        case 'vampire':
            setRule("IsVampire", "Equals", "true");
            break;
        case 'werebeast':
            setRule("IsWerebeast", "Equals", "true");
            break;
        case 'necromancer':
            setRule("IsNecromancer", "Equals", "true");
            break;
    }
}, { deep: true });

</script>