import { defineStore } from 'pinia'
import ProjectsService from '../api/ProjectsService'
import { ref } from 'vue'
import type { Project } from '../types'

export const useProjectStore = defineStore('project', () => {
  
  const projects = ref<Project[]>([])
  
  const fetchProjects = async () => {
    const res = await ProjectsService.getProjects()
    projects.value = res.data
  }

  const createProject = async (project: Partial<Project>) => {
    await ProjectsService.createProject(project)
  }

  const updateProject = async (project: Partial<Project>) => {
    await ProjectsService.updateProject(project)
  }

  const deleteProject = async (id: number) => {
      await ProjectsService.deleteProject(id)
  }

  return { projects, fetchProjects, createProject, updateProject, deleteProject }
})