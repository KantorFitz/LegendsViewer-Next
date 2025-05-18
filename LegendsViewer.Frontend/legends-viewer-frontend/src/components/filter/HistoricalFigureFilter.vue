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
        <v-divider class="mt-3 mb-3"/>
        <v-list-item>
            <v-checkbox style="float: left; margin: -10px;" label="Age" v-model="localRules.AgeRuleActive"></v-checkbox>
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
            <v-checkbox style="float: left; margin: -10px;" label="Born" v-model="localRules.BornRuleActive"></v-checkbox>
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
            <v-checkbox style="float: left; margin: -10px;" label="Died" v-model="localRules.DiedRuleActive"></v-checkbox>
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
    AgeRuleActive: false,
    AgeOperator: null as FilterOperator | null,
    AgeValue: null as number | null,
    BornRuleActive: false,
    BornOperator: null as FilterOperator | null,
    BornValue: null as number | null,
    DiedRuleActive: false,
    DiedOperator: null as FilterOperator | null,
    DiedValue: null as number | null
});

// Whenever filters change (or on mount), update localRules
watchEffect(() => {
    const filters = props.filters ?? [];

    const existsRule = (prop: string) => 
            filters.find(r => r.propertyName === prop) ? true : false;

    const findRuleValue = (prop: string) =>
        filters.find(r => r.propertyName === prop)?.value ?? '';

    const findRuleNumberValue = (prop: string) =>
        {
            var stringValue = filters.find(r => r.propertyName === prop)?.value;
            return stringValue ? parseInt(stringValue) : null;
        };

    const findRuleOperator = (prop: string) =>
        filters.find(r => r.propertyName === prop)?.operator ?? null;

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
    localRules.value.AgeRuleActive = existsRule("Age");
    localRules.value.AgeOperator = findRuleOperator("Age");
    localRules.value.AgeValue = findRuleNumberValue("Age");
    localRules.value.BornRuleActive = existsRule("BirthYear");
    localRules.value.BornOperator = findRuleOperator("BirthYear");
    localRules.value.BornValue = findRuleNumberValue("BirthYear");
    localRules.value.DiedRuleActive = existsRule("DeathYear");
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
            existing.operator = operator;
            existing.value = value;
        } else {
            const filterRule = {
                propertyName: propName,
                operator: operator,
                value: value
            }
            props.filters!.push(filterRule);
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
    if (localRules.value.AgeRuleActive == false) {
        removeRule("Age");
    } else if(localRules.value.AgeOperator != null && localRules.value.AgeValue != null) {
        setRule("Age", localRules.value.AgeOperator, localRules.value.AgeValue.toString());
    }

    // BORN filter logic
    if (localRules.value.BornRuleActive == false) {
        removeRule("BirthYear");
    } else if(localRules.value.BornOperator != null && localRules.value.BornValue != null) {
        setRule("BirthYear", localRules.value.BornOperator, localRules.value.BornValue.toString());
    }

    // DIED filter logic
    if (localRules.value.DiedRuleActive == false) {
        removeRule("DeathYear");
    } else if(localRules.value.DiedOperator != null && localRules.value.DiedValue != null) {
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