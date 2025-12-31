import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import 'dayjs/locale/da'
import isoWeek from 'dayjs/plugin/isoWeek'
import type { TimesheetRow, WeekDay, TeamCollection, DecisionPayload, TimeEntry, TimesheetPayload, ApiTeamCollection, Rows } from '../types'
import timesheetService from '@/api/timesheetService'
import timeEntriesService from '@/api/timeEntriesService'
import { useAuthStore } from './AuthStore'
import type { BlockLike } from 'typescript'

dayjs.locale('da')
dayjs.extend(isoWeek)

export const useTimesheetStore = defineStore('timesheet', () => {
  const createEmptyRow = (): Rows => {
    return {
      projectId: 0,
      hours: {}
    }
  }


  const myRows = ref<TimesheetRow>({ timesheetId: 0, rows: [] })
  const isApproved = ref(false)
  const currentTimesheetId = ref<number | null>(null)
  const currentTimesheetStatus = ref<number | null>(null)

  const addRow = () => {
    myRows.value.rows.push(createEmptyRow())
  }

  const removeRow = (index: number) => {
    myRows.value.rows.splice(index, 1)
  }

  const validateRows = (): boolean => {
    if (myRows.value.rows.length === 0 || !myRows.value.rows) {
      alert("Du skal have mindst én række i dit timesheet.")
      return false
    }
    for (const row of myRows.value.rows) {
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
  }

  const previousWeek = () => {
    currentWeekStart.value = currentWeekStart.value.subtract(1, 'week')
  }

  const weekHeader = computed(() => {
    const start = currentWeekStart.value.format('DD MMM')
    const end = currentWeekStart.value.add(6, 'day').format('DD MMM YYYY')
    const week = currentWeekStart.value.isoWeek()
    return `${start} - ${end} - uge ${week}`
  })

  // Manager timesheets
  const AuthStore = useAuthStore()
  const teamRows = ref<TeamCollection>({})

  const loadTeamRows = async () => {
    const startObj = currentWeekStart.value
    const endObj = startObj.endOf('isoWeek')
    const startStr = startObj.format('YYYY-MM-DD')
    const endStr = endObj.format('YYYY-MM-DD')

    const result = await timesheetService.getWeeklyTimeSheets(AuthStore.user?.userId ?? 0, startStr, endStr)

    const apiData = result.data.users as ApiTeamCollection | undefined

    const normalized: TeamCollection = {}

    if (apiData) {
      for (const [userName, timesheets] of Object.entries(apiData)) {

        normalized[userName] = timesheets.map(ts => ({
          timesheetId: ts.timesheetId,

          rows: ts.rows.map(row => ({
            projectId: row.project.projectId,
            hours: row.hours
          }))
        }))
      }
    }
    teamRows.value = normalized
  }

  const submitDecision = async (timesheetId: number, status: number, comment: string) => {
    const currentLeaderId = AuthStore.user?.userId ?? 0

    const payload: DecisionPayload = {
      timesheetId: timesheetId,
      leaderId: currentLeaderId,
      status: status,
      comment: comment,
    }
    await timesheetService.updateTimeSheet(payload)
    loadTeamRows()
  }

  const saveTimesheet = async (submit: boolean) => {
    if (!validateRows()) return

    try {
      const userId = AuthStore.user?.userId ?? 0
      let tsId = currentTimesheetId.value

      if (!tsId) {
        const startObj = currentWeekStart.value
        const endObj = startObj.endOf('isoWeek')

        const payload: TimesheetPayload = {
          userId: userId,
          periodStart: startObj.format('YYYY-MM-DD'),
          periodEnd: endObj.format('YYYY-MM-DD')
        }

        const timesheetRes = await timesheetService.createTimeSheet(payload)
        tsId = timesheetRes.data.timesheetId
        currentTimesheetId.value = tsId
      }

      const entriesToSave: TimeEntry[] = []

      for (const row of myRows.value.rows) {
        if (!row.projectId) continue

        for (const day of weekDays.value) {
          const hours = row.hours[day.fullDate]
          if (hours && hours > 0) {
            entriesToSave.push({
              userId: userId,
              projectId: row.projectId,
              date: day.fullDate,
              note: "",
              hours: hours,
              timesheetId: tsId!
            })
          }
        }
      }
      if (entriesToSave.length > 0) {
        await timeEntriesService.SaveTimeEntries(tsId ?? 0, entriesToSave)
      } else {
        await timeEntriesService.SaveTimeEntries(tsId ?? 0, [])
      }
      if (submit) {
        const payload: DecisionPayload = {
          timesheetId: tsId ?? 0,
          leaderId: 0,
          status: 0,
          comment: ""
        }
        await timesheetService.updateTimeSheet(payload)
        alert("Tid er indsendt til godkendelse.")
      }
      else {
        alert("Tid er gemt som kladde.")
      }


    } catch (error) {
      console.error("Fejl ved indsendelse", error)
      alert("Der skete en fejl. Prøv igen.")
    } finally {
      GetTimesheet()
    }
  }

  const GetTimesheet = async () => {
    try {

      const timesheet = await timesheetService.getUserTimeSheet(AuthStore.user?.userId ?? 0, currentWeekStart.value.format('YYYY-MM-DD'), currentWeekStart.value.endOf('isoWeek').format('YYYY-MM-DD'))
      const timeEntries = await timeEntriesService.GetTimeEntriesByTimesheetId(AuthStore.user?.userId ?? 0, timesheet.data.timesheetId)

      const rows: TimesheetRow = { timesheetId: timesheet.data.timesheetId, rows: [] }

      for (const entry of timeEntries.data) {
        let row = rows.rows.find(r => r.projectId === entry.projectId)
        if (!row) {
          row = {

            projectId: entry.projectId,
            hours: {}
          }
          rows.rows.push(row)
        }
        row.hours[entry.date] = entry.hours
      }
      myRows.value = rows
      isApproved.value = timesheet.data.status === 1
      currentTimesheetId.value = timesheet.data.timesheetId
      currentTimesheetStatus.value = timesheet.data.status
    } catch (error) {
      console.error("Fejl ved hentning af timesheet", error)
      myRows.value = { timesheetId: 0, rows: [] }
      currentTimesheetId.value = null
      isApproved.value = false
      currentTimesheetStatus.value = null
    }
  }

  return {
    myRows,
    teamRows,
    addRow,
    removeRow,
    isApproved,
    currentTimesheetId,
    currentTimesheetStatus,

    currentWeekStart,
    weekDays,
    weekHeader,
    nextWeek,
    previousWeek,
    setWeek,
    setWeekFromDate,

    loadTeamRows,
    submitDecision,
    saveTimesheet,
    GetTimesheet,
  }
})
