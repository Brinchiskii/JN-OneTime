import http from "./http";
import type { Project } from "../types/index";

export default {
    getProjects() {
        return http.get<Project[]>("/projects");
    },
    
    createProject(project: Partial<Project>) {
        return http.post<Project>("/projects", project);
    },
    
    updateProject(project: Partial<Project>) {
        return http.put<Project>(`/projects/${project.projectId}`, project);
    },
    
    deleteProject(id: number) {
        return http.delete(`/projects/${id}`);
    }
}