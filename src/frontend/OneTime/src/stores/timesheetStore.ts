import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import 'dayjs/locale/da'
import isoWeek from 'dayjs/plugin/isoWeek'
import type { TimesheetRow, WeekDay } from '../types'

dayjs.locale('da')
dayjs.extend(isoWeek)

export const useTimesheetStore = defineStore('timesheet', () => {
  // 1. STATE (Dataen)
  const rows = ref<TimesheetRow[]>([
    {
      projectId: null,
      hours: { mon: 0, tue: 0, wed: 0, thu: 0, fri: 0, sat: 0, sun: 0 },
    },
  ])

  // 2. ACTIONS (Funktioner der ændrer data)
  function addRow() {
    rows.value.push({
      projectId: null,
      hours: { mon: 0, tue: 0, wed: 0, thu: 0, fri: 0, sat: 0, sun: 0 },
    })
  }

  function removeRow(index: number) {
    rows.value.splice(index, 1)
  }

  const currentWeekStart = ref(dayjs().startOf('isoWeek'))
  const weekDays = computed<WeekDay[]>(() => {
    const days: WeekDay[] = []
    const keys = ['mon', 'tue', 'wed', 'thu', 'fri', 'sat', 'sun']

    for (let i = 0; i < 7; i++) {
      const dayToAdd = currentWeekStart.value.add(i, 'day')
      
      days.push({
        name: dayToAdd.format('ddd'),
        key: keys[i],
        date: dayToAdd.format('DD/MM'),
        fullDate: dayToAdd.format('YYYY-MM-DD'),
      })
    }
    return days
  })

  const weekHeader = computed(() => {
    const start = currentWeekStart.value.format('DD MMM')
    const end = currentWeekStart.value.add(6, 'day').format('DD MMM YYYY')
    const test = currentWeekStart.value.isoWeek()
    return `${start} - ${end} - uge ${test}`
  })

  const periodText = computed(() => {
    const start = currentWeekStart.value.format('DD MMM YYYY')
    const end = currentWeekStart.value.add(6, 'day').format('DD MMM YYYY')
    return `Periode: ${start} - ${end}`
  })

  function nextWeek() {
    currentWeekStart.value = currentWeekStart.value.add(1, 'week')
    // TODO: Her skal du senere kalde en funktion der henter nye data (rows) fra API'et
    // fetchWeekData()
  }

  function previousWeek() {
    currentWeekStart.value = currentWeekStart.value.subtract(1, 'week')
    // TODO: fetchWeekData()
  }

  return {
    rows,
    addRow,
    removeRow,

    currentWeekStart,
    weekDays,
    weekHeader,
    periodText,
    nextWeek,
    previousWeek,
  }
})
