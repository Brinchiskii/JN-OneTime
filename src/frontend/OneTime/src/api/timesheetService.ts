import http from './http'
import { type ApiRow, type DecisionPayload, type TimesheetPayload, type UsersCollection } from '@/types'
const base = "Timesheets"
export default {
  getWeeklyTimeSheet(id: number, start: string , end: string) {
    return http.get<Record<string, ApiRow[]>>(`/${base}/leader/${id}/team?startDate=${start}&endDate=${end}`);
  },

  updateTimeSheet(payload: DecisionPayload) {
    return http.post(`${base}/update/${payload.timesheetId}`, payload)
  },

  createTimeSheet(payload: TimesheetPayload){
    return http.post(`${base}/submit`, payload)
  }
}
