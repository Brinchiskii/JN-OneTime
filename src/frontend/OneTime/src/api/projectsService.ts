import https from "./Https";
import type { Project } from "../types/index";

export default {
    getProjects() {
        return https.get<Project[]>("/projects");
    },
    
    createProject(project: Partial<Project>) {
        return https.post<Project>("/projects", project);
    },
    
    updateProject(project: Partial<Project>) {
        return https.put<Project>(`/projects/${project.projectId}`, project);
    },
    
    deleteProject(id: number) {
        return https.delete(`/projects/${id}`);
    }
}