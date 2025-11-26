<script setup lang="ts">
import { computed, ref } from 'vue'
import type { TimesheetRow, Project, WeekDay } from '../types'

const props = defineProps<{
  rows: TimesheetRow[]
  weekDays: WeekDay[]
  projects: Project[]
}>()

const getRowTotal = (row: TimesheetRow) =>
  Object.values(row.hours).reduce((acc, val) => acc + (Number(val) || 0), 0)

const getColumnTotal = (dayKey: string) =>
  props.rows.reduce((acc, row) => acc + (Number(row.hours[dayKey]) || 0), 0)

const grandTotal = computed(() => props.rows.reduce((acc, row) => acc + getRowTotal(row), 0))
</script>

<template>
  <div class="table-responsive border rounded">
    <table class="table table-sm align-middle mb-0">
      <thead class="table-light">
        <tr>
          <th class="text-center">Projekt</th>
          <th class="text-center" v-for="day in props.weekDays" :key="day.key">
            {{ day.name }}<br /><small class="text-muted">{{ day.date }}</small>
          </th>
          <th class="text-center">Total</th>
          <th class="text-center">Handlinger</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, index) in props.rows" :key="index">
          <td>
            <select class="form-select">
              <option>Vælg projekt</option>
              <option v-for="p in props.projects" :key="p.id" :value="p.id">
                {{ p.name }}
              </option>
            </select>
          </td>

          <td v-for="day in props.weekDays" :key="day.key" class="text-center">
            <input
              type="number"
              v-model="row.hours[day.key]"
              class="form-control form-control-sm text-center w-75 mx-auto"
              placeholder="0"
              min="0"
              max="24"
              step="0.25"
            />
          </td>

          <td class="text-center">
            <span class="badge bg-secondary">{{ getRowTotal(row) }}t</span>
          </td>

          <td class="text-center">
            <button
              @click="rows.splice(index, 1)"
              class="btn btn-outline-danger btn-sm"
              title="Delete row"
            >
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>

        <tr class="table-info fw-semibold">
          <td class="text-end">Daglige timer:</td>

          <td v-for="day in weekDays" :key="day.key" class="text-center">
            <span class="badge bg-light text-dark">{{ getColumnTotal(day.key) }}t</span>
          </td>

          <td class="text-center">
            <span class="badge bg-dark">{{ grandTotal }}t</span>
          </td>
          <td></td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
