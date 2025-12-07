import http from './http'
import type { DecisionPayload, TimesheetRow, UsersCollection } from '@/types'

export default {
  getWeeklyTimeSheet(id: number, start: string , end: string) {
    return http.get<UsersCollection>(`/Timesheets/leader/${id}/team?startDate=${start}&endDate=${end}`);
  },
  updateTimeSheet(payload: DecisionPayload) {
    return http.post(`Timesheets/update/${payload.timesheetId}`, payload)
  }
}
