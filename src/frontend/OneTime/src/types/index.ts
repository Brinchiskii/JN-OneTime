export interface Project {
  projectId: number
  name: string
  status: number
  timeEntries: Array<number>
}

export interface TimeEntry {
  userId: number
  projectId: number
  note: string
  date: string
  hours: number
  timesheetId: number
}

export interface ApiRow {
  project: {
    projectId: number
    name: string
    status: number
  }
  hours: Record<string, number>
}

export interface TimesheetRow {
  projectId: number
  hours: Record<string, number>
}

export type UsersCollection = Record<string, TimesheetRow[]>

export interface WeekDay {
  name: string
  key: any
  date: string
  fullDate: string
}

export interface DecisionPayload {
  timesheetId: number
  leaderId: number
  status: number
  comment: string
}

export interface TimesheetPayload {
  userId: number
  periodStart: string
  periodEnd: string
}

export interface User {
  userId: number
  name: string
  email: string
  role: number
  managerId: number
}

export interface UserPayload{
  name: string
  email: string
  password: string | null
  role: number
  managerId: number | null
}
export interface Log {
  auditLogId: number;
  timestamp: string;
  action: string;
  entityType: string;
  entityId: number;
  actorUserId: number;
  actorUserName: string;
  details: string;
}
