import http from "./http";
import type { Project } from "../types/index";

export default {
    getProjects() {
        return http.get<Project[]>("/projects");
    },
    
}