<script>

import Loading from 'vue-loading-overlay';
import 'vue-loading-overlay/dist/css/index.css';

export default{
    data(){
        return{
            isLoading:false
        }
    },
    components:{
        Loading
    },
    methods:{
        StartLoading(){
            this.isLoading = true
        },
        StopLoading(){
            this.isLoading = false
        }
    },
    async mounted(){

        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
        })
    }
}
</script>

<template>
    <nav
        class="navbar navbar-expand-sm navbar-light bg-light mt-0"
    >
        <div class="container">
            <a class="navbar-brand">УБС</a>
            <button
                class="navbar-toggler d-lg-none"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#collapsibleNavId"
                aria-controls="collapsibleNavId"
                aria-expanded="false"
                aria-label="Toggle navigation"
            >
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="collapsibleNavId">
                <ul class="navbar-nav me-auto mt-2 mt-lg-0">
                    <router-link :to="{name: 'communications'}" class="nav-item nav-link" >Соединения </router-link>
                    <router-link :to="{name: 'connections'}" class="nav-item nav-link" >Подключения </router-link>
                    <router-link :to="{name: 'chanels'}" class="nav-item nav-link" >Каналы </router-link>
                    <router-link :to="{name: 'about'}" class="nav-item nav-link" >О программе </router-link>
                </ul>
                
            </div>
        </div>
    </nav>
    <loading 
        :active="isLoading"
        :is-full-page="true"
        loader="dots"
    />
    <div class="mx-1">
        <router-view
            @StartLoading="StartLoading"
            @StopLoading="StopLoading"
         />
    </div>

</template>

<style scoped>
</style>
