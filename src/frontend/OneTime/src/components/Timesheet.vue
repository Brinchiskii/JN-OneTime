<script setup lang="ts">
import { computed, onMounted, ref, onBeforeMount } from 'vue'
import type { TimesheetRow, Project, WeekDay } from '../types'
import { useProjectStore } from '@/stores/projectStore'
import { useTimesheetStore } from '@/stores/timesheetStore'

const props = defineProps<{
  timesheetrows: TimesheetRow
  weekDays: WeekDay[]
  readonly?: boolean
}>()

const rows = computed(() => props.timesheetrows?.rows)
const projectStore = useProjectStore()
const timesheetStore = useTimesheetStore()

onMounted(async () => {
  await projectStore.fetchProjects()
})

const getColumnTotal = (dayKey: string) =>
  props.timesheetrows.rows?.reduce((acc, row) => acc + (Number(row.hours[dayKey]) || 0), 0)

const getRowTotal = (row: any) => {
  if (!row || !row.hours) return 0

  const total = Object.values(row.hours).reduce((acc: any, val: any) => {
    return acc + (Number(val) || 0)
  }, 0)

  return total
}

const grandTotal = computed(() => {
  return props.timesheetrows.rows.map(row => getRowTotal(row)).reduce((acc: any, val: any) => acc + val, 0)
})

</script>

<template>
  <div class="timesheet-card shadow">
    <table class="custom-table">
      <thead>
        <tr>
          <th style="width: 25%; padding-left: 20px">Projekt</th>
          <th class="text-center" v-for="day in props.weekDays" :key="day.key">
            {{ day.name }}
            <span class="d-block fw-normal text-muted" style="font-size: 0.7rem">{{
              day.date
            }}</span>
          </th>
          <th class="text-center" style="width: 8%">Total</th>
          <th style="width: 5%" v-if="!props.readonly">Handlinger</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, index) in rows" :key="row.projectId" class="project-row">
          <td>
            <select class="project-select form-select" :disabled="props.readonly" v-model="row.projectId">
              <option :value="0" hidden>Vælg projekt</option>
              <option v-for="p in projectStore.projects" :key="p.projectId" :value="p.projectId">
                {{ p.name }}
              </option>
            </select>
          </td>

          <td v-for="day in props.weekDays" :key="day.key" class="text-center">
            <input type="number" v-model="row.hours[day.fullDate]" :class="!readonly ? 'day-input' : 'hours-cell'"
              placeholder="0" min="0" max="24" step="0.25" :readonly="props.readonly" :disabled="props.readonly" />
          </td>

          <td class="text-center">
            <span class="badge bg-secondary">{{ getRowTotal(row) }}t</span>
          </td>

          <td class="text-center" v-if="!props.readonly">
            <button @click="timesheetStore.removeRow(index)" class="btn btn-outline-danger btn-sm" title="Delete row">
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>
        <tr v-if="rows?.length === 0">
          <td colspan="9" class="text-center py-4 text-muted fst-italic">Ingen registreringer.</td>
        </tr>
        <tr class="table-info fw-semibold">
          <td class="text-end">Daglige timer:</td>

          <td v-for="day in weekDays" :key="day.key" class="text-center">
            <span class="badge bg-light text-dark">{{ getColumnTotal(day.fullDate) }}t</span>
          </td>

          <td class="text-center">
            <span class="badge bg-dark">{{ grandTotal }}t</span>
          </td>
          <td v-if="!props.readonly"></td>
        </tr>
      </tbody>
    </table>
  </div>
  <button v-if="!props.readonly" @click="timesheetStore.addRow" class="btn btn-outline-secondary w-100 mt-3">
    <i class="bi bi-plus-lg me-1"></i> Tilføj nyt projekt
  </button>
</template>

<style scoped>
.timesheet-card {
  background: white;
  border-radius: 16px;
  box-shadow: var(--card-shadow);
  overflow: hidden;
  /* Sikrer at indhold ikke stikker ud af de runde hjørner */
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.project-row td {
  padding: 10px 5px;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: middle;
}

.project-row:last-child td {
  border-bottom: none;
}

.project-row:hover {
  background-color: #fafafa;
}

.day-input {
  border: 1px solid #e5e7eb;
  background: white;
  border-radius: 6px;
  text-align: center;
  font-weight: 500;
  color: #374151;
  width: 100%;
  padding: 8px 0;
  transition: all 0.2s;
  font-size: 0.95rem;
  min-width: 50px;
}

.day-input:focus {
  background: white;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  outline: none;
}

.day-input:hover {
  border-color: #d1d5db;
}

.day-input.weekend {
  background-color: #f9fafb;
  color: #9ca3af;
}

.hours-cell {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 6px 0;
  text-align: center;
  font-weight: 500;
  width: 50px;
  display: inline-block;
  color: #475569;
}

.hours-cell.violation {
  background-color: #fef2f2;
  color: #dc2626;
  border-color: #fca5a5;
  font-weight: 700;
}

.project-select {
  font-weight: 600;
  color: #1f2937;
  padding: 8px 5px;
  cursor: pointer;
  width: 100%;
  background: transparent;
}

.project-select:focus {
  outline: none;
  box-shadow: none;
}
</style>
