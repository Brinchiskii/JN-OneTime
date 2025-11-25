export interface ApiTimeEntry {
  id: number;
  projectId: number;
  date: string; 
  hours: number;
}

export interface TimesheetRow {
    projectId: number | null;
    hours: { [key: string]: number };
}

export interface Project {
  id: number
  name: string
  code: string 
}

export interface WeekDay {
  name: string
  key: any
  date: string
  fullDate: string
}