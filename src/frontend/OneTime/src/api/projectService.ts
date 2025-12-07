import https from "./https";
import type { Project } from "../types/index";

export default {
    getProjects() {
        return https.get<Project[]>("/Timeentries/projects");
    },
    
}