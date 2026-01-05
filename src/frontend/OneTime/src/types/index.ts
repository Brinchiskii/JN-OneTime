export interface Project {
  projectId: number
  name: string
  status: number
}

export interface TimeEntry {
  userId: number
  projectId: number
  note: string
  date: string
  hours: number
  timesheetId: number
}

export interface TimesheetRow {
  timesheetId: number
  status: number
  comment: string | null
  rows: Rows[]
}

export interface ApiTimesheetRow {
  timesheetId: number
  status: number
  comment: string | null
  rows: ApiRows[]
}

export interface Rows {
  projectId: number
  hours: Record<string, number>
}

interface ApiRows {
  project: {
    projectId: number
    name: string
    status: number
  }
  hours: Record<string, number>
}

export type TeamCollection = Record<string, TimesheetRow[]>
export type ApiTeamCollection = Record<string, ApiTimesheetRow[]>

export interface WeekDay {
  name: string
  key: any
  date: string
  fullDate: string
}

export interface DecisionPayload {
  timesheetId: number
  leaderId: number | null
  status: number | null
  comment: string | null
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

export interface UserPayload {
  name: string
  email: string
  password: string | null
  role: number
  managerId: number | null
}

export interface UserLogin {
  token: string
  userId: number
  name: string
  email: string
  role: number
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

export interface JwtPayload {
  // NameIdentifier
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string

  // Name
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string

  // Role
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string | string[]

  // Email
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": string

  exp: number // Udløbstid (standard)
}

export interface Timesheet {
  timesheetId: number
  userId: number
  periodStart: string
  periodEnd: string
  status: number
  decidedAt: string | null
  comment: string | null
}

interface ProjectMember {
  userId: number
  name: string
  hoursContributor: number
}

export interface ProjectStat {
  projectId: number      
  projectName: string    
  status: number         
  totalHours: number     
  members: ProjectMember[]
}

