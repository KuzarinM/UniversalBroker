<script>
import CommunicationApiMixine from "./mixines/CommunicationApiMixine"

export default{
    data(){
        return{
            data:{}
        }
    },
    mixins:[
        CommunicationApiMixine
    ],
    async mounted(){
        this.data = await (await this.GetCommunicationList()).json();

        console.log(this.data)
    }
}
</script>

<template>
 <div>{{ this.data }}</div> 
</template>

<style scoped>
</style>
