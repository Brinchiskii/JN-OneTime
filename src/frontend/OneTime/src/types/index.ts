export interface Project {
  projectId: number
  name: string
  status: number
  timeEntries: Array<number>
}

// export interface TimeEntry {
//   id: number;
//   project: Project; 
//   date: string; 
//   hours: number;
// }

export interface TimesheetRow {
    projectId: number;
    hours: Record<string, number>
}

export type UsersCollection = Record<string, TimesheetRow[]>;

export interface WeekDay {
  name: string
  key: any
  date: string
  fullDate: string
}