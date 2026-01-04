import http from './http'
import type { ApiTimesheetRow, DecisionPayload, Timesheet, TimesheetPayload, TeamCollection } from '@/types'
const base = "Timesheets"
export default {
  getWeeklyTimeSheets(id: number, startDate: string , endDate: string) {
    return http.get<Record<string, ApiTimesheetRow[]>>(`/${base}/leader/${id}/team?startDate=${startDate}&endDate=${endDate}`);
  },

  updateTimeSheet(payload: DecisionPayload) {
    return http.post(`${base}/update/${payload.timesheetId}`, payload)
  },

  createTimeSheet(payload: TimesheetPayload){
    return http.post(`${base}/submit`, payload)
  },
  
  getUserTimeSheet(userId: number, startDate: string , endDate: string) {
    return http.get<Timesheet>(`/${base}/user/${userId}/time?startDate=${startDate}&endDate=${endDate}`);
  }
}
