import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import 'dayjs/locale/da'
import isoWeek from 'dayjs/plugin/isoWeek'
import type { TimesheetRow, WeekDay, UsersCollection, DecisionPayload } from '../types'
import api from '@/api/TimeRegistration'

dayjs.locale('da')
dayjs.extend(isoWeek)

export const useTimesheetStore = defineStore('timesheet', () => {

  const createEmptyRow = (): TimesheetRow => {
    return {
      projectId: 0,
      hours: { mon: 0, tue: 0, wed: 0, thu: 0, fri: 0, sat: 0, sun: 0 },
    }
  }

  const myRows = ref<TimesheetRow[]>([createEmptyRow()])

  const addRow = () => {
    myRows.value.push(createEmptyRow())
  }

  const removeRow = (index: number) => {
    myRows.value.splice(index, 1)
  }

  const getTotalHours = (row: TimesheetRow) => {
    return Object.values(row.hours).reduce((acc, val) => acc + (Number(val) || 0), 0)
  }

  // Date management
  const currentWeekStart = ref(dayjs().startOf('isoWeek'))

  const weekDays = computed<WeekDay[]>(() => {
    const keys = ['mon', 'tue', 'wed', 'thu', 'fri', 'sat', 'sun']

    return keys.map((key, index) => {
      const dayToAdd = currentWeekStart.value.add(index, 'day')
      return {
        key,
        name: dayToAdd.format('ddd'),
        date: dayToAdd.format('DD/MM'),
        fullDate: dayToAdd.format('YYYY-MM-DD'),
      }
    })
  })

  const setWeekFromDate = (date: any) => {
    const week = dayjs(date).isoWeek()
    setWeek(week)
  }

  const setWeek = (isoWeekNumber: number = dayjs().isoWeek(), year: number = dayjs().year()) => {
    currentWeekStart.value = dayjs().year(year).isoWeek(isoWeekNumber).startOf('isoWeek')
  }

  const nextWeek = () => {
    currentWeekStart.value = currentWeekStart.value.add(1, 'week')
    // TODO: Her skal vi senere kalde en funktion der henter nye data (rows) fra API'et
    // fetchWeekData()
  }
  
  const previousWeek = () => {
    currentWeekStart.value = currentWeekStart.value.subtract(1, 'week')
    // TODO: fetchWeekData()
  }

  const weekHeader = computed(() => {
    const start = currentWeekStart.value.format('DD MMM')
    const end = currentWeekStart.value.add(6, 'day').format('DD MMM YYYY')
    const week = currentWeekStart.value.isoWeek()
    return `${start} - ${end} - uge ${week}`
  })
  
  // Manager timesheets
  const teamRows = ref<UsersCollection>({})

  const loadTeamRows = async () => {
    const startObj = currentWeekStart.value
    const endObj = startObj.endOf('isoWeek')
    const startStr = startObj.format('YYYY-MM-DD')
    const endStr = endObj.format('YYYY-MM-DD')

    const result = await api.getWeeklyTimeSheet(4, startStr, endStr)

    const usersData = result.data?.users

    const normalized: UsersCollection = {}
    if(usersData)
    for (const [user, rows] of Object.entries(usersData)) {
      normalized[user] = rows.map((row) => ({
        projectId: row.project.projectId,
        hours: row.hours,
      }))
    }
    teamRows.value = normalized
  }

  const submitDecision = async (timesheetId: number, status: number, comment: string,) => {
    
    const currentLeaderId = 4;

    const payload: DecisionPayload = {
      timesheetId: timesheetId,
      leaderId: currentLeaderId,
      status: status,
      comment: comment
    }
    await api.updateTimeSheet(payload)
  }

  return {
    myRows,
    teamRows,
    addRow,
    removeRow,
    getTotalHours,

    currentWeekStart,
    weekDays,
    weekHeader,
    nextWeek,
    previousWeek,
    setWeek,
    setWeekFromDate,
    
    loadTeamRows,
    submitDecision,
  }
})
