import https from './Https'
import type { ApiTimesheetRow, DecisionPayload, Timesheet, TimesheetPayload, TeamCollection } from '@/types'
const base = "Timesheets"
export default {
  getWeeklyTimeSheets(id: number, startDate: string , endDate: string) {
    return https.get<Record<string, ApiTimesheetRow[]>>(`/${base}/leader/${id}/team?startDate=${startDate}&endDate=${endDate}`);
  },

  updateTimeSheet(payload: DecisionPayload) {
    return https.post(`${base}/update/${payload.timesheetId}`, payload)
  },

  createTimeSheet(payload: TimesheetPayload){
    return https.post(`${base}/submit`, payload)
  },
  
  getUserTimeSheet(userId: number, startDate: string , endDate: string) {
    return https.get<Timesheet>(`/${base}/user/${userId}/time?startDate=${startDate}&endDate=${endDate}`);
  }
}
