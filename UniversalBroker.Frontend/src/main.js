import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createMemoryHistory, createWebHistory, createRouter } from 'vue-router'
import ConnectionsPage from './pages/ConnectionsPage.vue'
import CommunicationsPage from './pages/CommunicationsPage.vue'
import ChanelsPage from './pages/ChanelsPage.vue'
import AboutPage from './pages/AboutPage.vue'


const routes = [
    { path: '/', redirect:"communications"},
    { path: '/communications', name:"communications", component: CommunicationsPage},
    { path: '/connections', name:"connections", component: ConnectionsPage},
    { path: '/chanels', name:"chanels", component: ChanelsPage},
    {path:'/about', name:"about", component:AboutPage}
  ]

const router = createRouter({
    //history: createMemoryHistory(),
    history: createWebHistory(),
    routes
  })

createApp(App).use(router).mount('#app')
