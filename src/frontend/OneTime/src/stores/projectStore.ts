import { defineStore } from 'pinia'
import projectsService from '../api/projectsService'
import { ref } from 'vue'
import type { Project } from '../types'

export const useProjectStore = defineStore('project', () => {
  
  const projects = ref<Project[]>([])
  
  const fetchProjects = async () => {
    const res = await projectsService.getProjects()
    projects.value = res.data
  }

  const createProject = async (project: Partial<Project>) => {
    await projectsService.createProject(project)
  }

  const updateProject = async (project: Partial<Project>) => {
    await projectsService.updateProject(project)
  }

  const deleteProject = async (id: number) => {
    await projectsService.deleteProject(id)
  }

  return { projects, fetchProjects, createProject, updateProject, deleteProject }
})