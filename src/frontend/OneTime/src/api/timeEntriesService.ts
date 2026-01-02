import http from "./http";
import type { TimeEntry } from "../types/index";

const base = "TimeEntries"

export default {
    CreateTimeEntry(payload: TimeEntry) {
        return http.post<TimeEntry[]>("/TimeEntries", payload);
    },
    GetTimeEntriesByTimesheetId(userId: number, timesheetId: number) {
        return http.get<TimeEntry[]>(`/${base}/user/${userId}/timesheet/${timesheetId}`);
    },
    SaveTimeEntries(timesheetId: number, payload: TimeEntry[]) {
        return http.post<TimeEntry[]>(`/${base}/bulk/${timesheetId}`, payload);
    }
}