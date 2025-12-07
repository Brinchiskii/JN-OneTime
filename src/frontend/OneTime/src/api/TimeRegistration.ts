import https from './https'
import type { TimesheetRow, UsersCollection } from '@/types'

export default {
  getWeeklyTimeSheet(id: number, start: string , end: string) {
    return https.get<UsersCollection>(`/Timesheets/leader/${id}/team?startDate=${start}&endDate=${end}`);
  },
}
