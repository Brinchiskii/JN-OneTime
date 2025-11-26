// src/stores/projectStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Project } from '../types'

export const useProjectStore = defineStore('project', () => {
  const projects = ref<Project[]>([])

  async function fetchProjects() {
    // Senere: const res = await axios.get('/api/projects')
    
    // Mock data
    projects.value = [
      { id: 101, name: 'Webshop Redesign', code: 'WEB' },
      { id: 102, name: 'Internt System', code: 'INT' },
      { id: 103, name: 'Support & Drift', code: 'SUP' },
    ]
  }

  return { projects, fetchProjects }
})