// src/stores/projectStore.ts
import { defineStore } from 'pinia'
import projectService from '../api/projectService'
import { ref } from 'vue'
import type { Project } from '../types'

export const useProjectStore = defineStore('project', () => {
  const projects = ref<Project[]>([])
  
  async function fetchProjects() {
    const res = await projectService.getProjects()
    projects.value = res.data
  }

  return { projects, fetchProjects }
})