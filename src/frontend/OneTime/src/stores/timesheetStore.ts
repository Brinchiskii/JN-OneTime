import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import 'dayjs/locale/da'
import isoWeek from 'dayjs/plugin/isoWeek'
import type { TimesheetRow, WeekDay, UsersCollection, DecisionPayload, TimeEntry, TimesheetPayload, ApiRow } from '../types'
import timesheetService from '@/api/timesheetService'
import timeEntriesService from '@/api/timeEntriesService'

dayjs.locale('da')
dayjs.extend(isoWeek)

export const useTimesheetStore = defineStore('timesheet', () => {
  const createEmptyRow = (): TimesheetRow => {
    return {
      projectId: 0,
      hours: { },
    }
  }

  const myRows = ref<TimesheetRow[]>([createEmptyRow()])

  const addRow = () => {
    myRows.value.push(createEmptyRow())
  }

  const removeRow = (index: number) => {
    myRows.value.splice(index, 1)
  }

  const validateRows = (): boolean => {
  for (const row of myRows.value) { 
    if (!row.projectId || row.projectId === 0) {
      alert("Du har en række, hvor der mangler at blive valgt et projekt.")
      return false
    }
    const hasHours = Object.values(row.hours).some(hours => hours > 0)

    if (!hasHours) {
      alert("Du har valgt et projekt, men ikke skrevet nogen timer på det.")
      return false
    }
  }
  return true
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

  const setWeekFromDate = (date: Date) => {
    const week = dayjs(date).isoWeek()
    const year = dayjs(date).year()
    setWeek(week, year)
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

    const result = await timesheetService.getWeeklyTimeSheet(4, startStr, endStr)

    const usersData = result.data?.users as Record<string, ApiRow[]> | undefined

    const normalized: UsersCollection = {}
    if (usersData)
      for (const [user, rows] of Object.entries(usersData)) {
        normalized[user] = rows.map(row => ({
          projectId: row.project.projectId,
          hours: row.hours,
        }))
      }
    teamRows.value = normalized
  }

  const submitDecision = async (timesheetId: number, status: number, comment: string) => {
    const currentLeaderId = 4

    const payload: DecisionPayload = {
      timesheetId: timesheetId,
      leaderId: currentLeaderId,
      status: status,
      comment: comment,
    }
    await timesheetService.updateTimeSheet(payload)
  }

  const submitTimesheet = async () => {

    if(!validateRows()) return

    const startObj = currentWeekStart.value
    const endObj = startObj.endOf('isoWeek')
    const startStr = startObj.format('YYYY-MM-DD')
    const endStr = endObj.format('YYYY-MM-DD')

    const payload: TimesheetPayload = {
      userId: 3,
      periodStart: startStr,
      periodEnd: endStr
    }
    
    const timesheet = await timesheetService.createTimeSheet(payload)

    const apiCalls: Promise<any>[] = []

    for (const row of myRows.value) {
      if (!row.projectId) continue

      for (const day of weekDays.value) {
        const hours = row.hours[day.fullDate]

        if (hours && hours > 0) {
          const payload: TimeEntry = {
            userId: timesheet.data.userId,
            projectId: row.projectId,
            date: day.fullDate,
            note: "",
            hours: hours,
            timesheetId: timesheet.data.timesheetId
          }
          apiCalls.push(timeEntriesService.CreateTimeEntry(payload))
        }
      }
    }

    try {
      await Promise.all(apiCalls)
      alert("Tidsregistrering sendt!")
    } catch (error) {
      console.error("Fejl ved indsendelse", error)
    }
  }

  return {
    myRows,
    teamRows,
    addRow,
    removeRow,

    currentWeekStart,
    weekDays,
    weekHeader,
    nextWeek,
    previousWeek,
    setWeek,
    setWeekFromDate,

    loadTeamRows,
    submitDecision,
    submitTimesheet,
  }
})
