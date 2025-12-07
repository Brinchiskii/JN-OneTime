<script setup lang="ts">
import Timesheet from '../components/Timesheet.vue'
import { onMounted, ref} from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import type { TimesheetRow } from '@/types'
import ManagerTeamCard from '@/components/ManagerTeamCard.vue'
import DatePicker from '@/components/DatePicker.vue' 

const timesheetStore = useTimesheetStore()

onMounted(() => {
  timesheetStore.loadTeamRows()
})

</script>

<template>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto">
    <h5 class="my-1">{{ timesheetStore.weekHeader }}</h5>
    <DatePicker @change="timesheetStore.loadTeamRows" @click="timesheetStore.loadTeamRows"></DatePicker>
    <ManagerTeamCard
      v-for="[userName, rows] in Object.entries(timesheetStore.teamRows)"
      :key="userName"
      :userName="userName"
      :rows="rows"
    >
    </ManagerTeamCard>
  </div>
</template>

<style scoped>

</style>
