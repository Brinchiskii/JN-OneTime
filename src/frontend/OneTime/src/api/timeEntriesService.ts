import https from "./Https";
import type { TimeEntry } from "../types/index";

const base = "TimeEntries"

export default {
    CreateTimeEntry(payload: TimeEntry) {
        return https.post<TimeEntry[]>("/TimeEntries", payload);
    },
    GetTimeEntriesByTimesheetId(userId: number, timesheetId: number) {
        return https.get<TimeEntry[]>(`/${base}/user/${userId}/timesheet/${timesheetId}`);
    },
    SaveTimeEntries(timesheetId: number, payload: TimeEntry[]) {
        return https.post<TimeEntry[]>(`/${base}/bulk/${timesheetId}`, payload);
    }
}