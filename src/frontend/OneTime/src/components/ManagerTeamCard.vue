<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useTimesheetStore } from '../stores/TimesheetStore'
import Timesheet from './Timesheet.vue'
import type { TimesheetRow } from '@/types'

const props = defineProps<{
  userName: string
  rows: TimesheetRow
  status: number
  comment: string | null
}>()

const Status = computed(() => {
  switch (props.status) {
    case 0: // Afventer
      return {
        text: "Afventer",
        class: "bg-warning text-dark", 
        icon: "bi bi-hourglass-split"
      };
    case 1: // Godkendt
      return {
        text: "Godkendt",
        class: "bg-success",
        icon: "bi bi-check-circle-fill"
      };
    case 2: // Afvist
      return {
        text: "Afvist",
        class: "bg-danger",
        icon: "bi bi-exclamation-circle-fill"
      };
    case 3: // Kladde
      return {
        text: "Kladde",
        class: "bg-secondary", 
        icon: "bi bi-pencil-square"
      };
    default: // Null / Ikke oprettet
      return {
        text: "Ikke oprettet",
        class: "bg-light text-muted border", 
        icon: "bi bi-circle", 
      };
  }
});

const timesheetStore = useTimesheetStore()
const comment = ref("")

const approve = () => {
  if (!props.rows?.timesheetId) return
  timesheetStore.submitDecision(props.rows.timesheetId, 1, comment.value)
  alert("Timesheet " + props.rows.timesheetId + " er blevet godkendt")
  emit('refresh')
}

const deny = () => {
  if (!props.rows?.timesheetId) return
  timesheetStore.submitDecision(props.rows.timesheetId, 2, comment.value)
  alert("Timesheet " + props.rows.timesheetId + " er blevet afvist")
  emit('refresh')
}

const emit = defineEmits<{
  (e: 'refresh'): void
}>()
</script>

<template>
  <div class="manager-card">
    <div class="card-header-custom">
      <div class="d-flex align-items-center gap-3">
        <div class="avatar">{{ userName.charAt(0) }}</div>
        <div class="fw-bold fs-5">{{ userName }}</div>
        <span class="badge d-inline-flex align-items-center gap-1 py-2 px-3" :class="Status.class">
          <i :class="Status.icon"></i>
          <span>{{ Status.text }}</span>
        </span>
      </div>
      <div v-if="props.status == 0" class="d-flex align-items-center">
        <input v-model="comment" type="text" placeholder="Tilføj kommentar..." />
        <button class="btn btn-success btn-sm px-3 ms-2" @click="approve()">
          <i class="bi bi-check-lg me-1"></i> <span>Godkend</span>
        </button>
        <button class="btn btn-danger btn-sm px-3 ms-2" @click="deny()">
          <i class="bi bi-check-lg me-1"></i> <span>Afvis</span>
        </button>
      </div>
      <div v-if="props.comment">{{ props.comment }}</div>
    </div>
    <Timesheet :timesheetrows="rows" :weekDays="timesheetStore.weekDays" :readonly="true"></Timesheet>
  </div>
</template>

<style scoped>
.box {
  max-width: 668px;
}

.manager-card {
  background: white;
  border-radius: 12px;
  box-shadow: var(--card-shadow);
  border: 1px solid rgba(0, 0, 0, 0.05);
  margin-bottom: 2rem;
  overflow: hidden;
}

.card-header-custom {
  background: white;
  padding: 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
