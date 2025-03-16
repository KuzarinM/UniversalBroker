<script>
import ChanelLogsModal from './components/modals/ChanelLogsModal.vue'

export default{
    components:{
        ChanelLogsModal
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
    <router-view />
</template>

<style scoped>
</style>
