import type { FilterRuleDto, FilterOperator } from '../stores/worldObjectStores';

export function existsRule(filters: FilterRuleDto[], prop: string) {
    return filters.some(r => r.propertyName === prop);
}

export function findRuleValue(filters: FilterRuleDto[], prop: string) {
    return filters.find(r => r.propertyName === prop)?.value ?? '';
}

export function findRuleNumberValue(filters: FilterRuleDto[], prop: string) {
    const stringValue = filters.find(r => r.propertyName === prop)?.value;
    return stringValue ? parseInt(stringValue) : null;
}

export function findRuleOperator(filters: FilterRuleDto[], prop: string) {
    return filters.find(r => r.propertyName === prop)?.operator ?? null;
}

export function removeRule(filters: FilterRuleDto[], propName: string) {
    const index = filters.findIndex(r => r.propertyName === propName);
    if (index !== -1) filters.splice(index, 1);
}

export function setRule(
    filters: FilterRuleDto[],
    propName: string,
    operator: FilterOperator,
    value: string
) {
    const existing = filters.find(r => r.propertyName === propName);
    if (existing) {
        existing.operator = operator;
        existing.value = value;
    } else {
        filters.push({
            propertyName: propName,
            operator,
            value
        });
    }
}

export function updateBoolRule(filters: FilterRuleDto[], localValue: string, propName: string) {
    if (localValue === '') {
        removeRule(filters, propName);
    } else {
        setRule(filters, propName, "Equals", localValue);
    }
}

export function updateNumberRule(
    filters: FilterRuleDto[],
    active: boolean,
    operator: FilterOperator | null,
    value: number | null,
    propName: string
) {
    if (!active) {
        removeRule(filters, propName);
    } else if (operator != null && value != null) {
        setRule(filters, propName, operator, value.toString());
    }
}