// src/stores/projectStore.ts
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

  return { projects, fetchProjects }
})