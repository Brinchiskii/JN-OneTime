import http from "./http";
import type { TimeEntry } from "../types/index";

const base = "TimeEntries"

export default {
    CreateTimeEntry(payload: TimeEntry) {
        return http.post<TimeEntry[]>("/TimeEntries", payload);
    },
}