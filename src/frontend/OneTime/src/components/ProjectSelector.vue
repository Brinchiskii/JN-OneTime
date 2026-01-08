<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'

const props = defineProps<{
  modelValue: number
  projects: any[]
  disabled?: boolean
  placeholder?: string
  readonly?: boolean
}>()

const emit = defineEmits(['update:modelValue'])

const isOpen = ref(false)
const searchQuery = ref('')
const inputRef = ref<HTMLElement | null>(null)
const wrapperRef = ref<HTMLElement | null>(null)
const dropdownRef = ref<HTMLElement | null>(null) 

const dropdownStyle = ref({ top: '0px', left: '0px', width: '0px' })

const filteredOptions = computed(() => {
  const query = searchQuery.value.toLowerCase()
  return props.projects.filter(p => 
    p.name.toLowerCase().includes(query)
  )
})

const selectedName = computed(() => {
  const found = props.projects.find(p => p.projectId === props.modelValue)
  return found ? found.name : ''
})

watch(() => props.modelValue, () => {
  if (!isOpen.value) {
    searchQuery.value = selectedName.value
  }
}, { immediate: true })

const updatePosition = () => {
  if (inputRef.value) {
    const rect = inputRef.value.getBoundingClientRect()
    dropdownStyle.value = {
      top: `${rect.bottom}px`, 
      left: `${rect.left}px`,
      width: `${rect.width}px`
    }
  }
}

const handleFocus = () => {
  if (props.disabled || props.readonly) return
  updatePosition()
  isOpen.value = true
  searchQuery.value = ''
}

const handleInput = () => {
  if (!isOpen.value) {
    isOpen.value = true
    updatePosition()
  }
}

const selectProject = (project: any) => {
  emit('update:modelValue', project.projectId)
  searchQuery.value = project.name
  isOpen.value = false
}

const handleClickOutside = (event: MouseEvent) => {
  if (wrapperRef.value && !wrapperRef.value.contains(event.target as Node)) {
    if (dropdownRef.value && dropdownRef.value.contains(event.target as Node)) {
        return 
    }
    
    isOpen.value = false
    searchQuery.value = selectedName.value
  }
}

const handleScroll = (event: Event) => {
  if (isOpen.value) {
    if (dropdownRef.value && dropdownRef.value.contains(event.target as Node)) {
        return
    }
    
    isOpen.value = false
    if (props.modelValue) searchQuery.value = selectedName.value
  }
}

onMounted(() => { 
  document.addEventListener('click', handleClickOutside)
  window.addEventListener('resize', handleScroll)
  window.addEventListener('scroll', handleScroll, true) 
})

onUnmounted(() => { 
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('resize', handleScroll)
  window.removeEventListener('scroll', handleScroll, true)
})
</script>

<template>
  <div ref="wrapperRef"> 
    
    <input 
        ref="inputRef"
        type="text" 
        class="form-control"
        :class="{'bg-light': disabled || readonly}"
        :placeholder="selectedName || placeholder || 'Vælg projekt...'"
        v-model="searchQuery"
        @focus="handleFocus"
        @input="handleInput" 
        :disabled="disabled || readonly"
        autocomplete="off"
    />

    <ul 
        v-if="isOpen" 
        ref="dropdownRef"
        class="dropdown-menu show shadow-sm border mt-1" 
        @mousedown.prevent 
        :style="{
            position: 'fixed',
            top: dropdownStyle.top,
            left: dropdownStyle.left,
            width: dropdownStyle.width,
            zIndex: 9999,
            maxHeight: '250px',
            overflowY: 'auto'
        }"
    >
        <li v-if="filteredOptions.length === 0" class="dropdown-item text-muted fst-italic disabled">
            Ingen fundet...
        </li>

        <li v-for="p in filteredOptions" :key="p.projectId">
            <button 
                class="dropdown-item d-flex justify-content-between align-items-center" 
                @mousedown.prevent="selectProject(p)"
                :class="{'active': p.projectId === modelValue}"
                type="button"
            >
                {{ p.name }}
                <i v-if="p.projectId === modelValue" class="bi bi-check-lg"></i>
            </button>
        </li>
    </ul>

  </div>
</template>

<style scoped>
.dropdown-item {
    cursor: pointer;
    padding: 8px 12px;
    font-size: 0.9rem;
}
.dropdown-item.active {
    background-color: #0d6efd;
    color: white;
}
</style>