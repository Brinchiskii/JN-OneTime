import http from "./http";
import type { ProjectStat } from "@/types"

const dashboard = '/dashboard'

export default {
    getDashboardStats(managerId: number, startDate: string, endDate: string) {
    return http.get<ProjectStat[]>(dashboard + `/stats/leader/${managerId}?startdate=${startDate}&enddate=${endDate}`);
    }
}