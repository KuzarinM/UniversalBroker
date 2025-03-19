<script>
import { TriStateCheckbox } from "vue-tri-state-checkbox";
import Multiselect from '@vueform/multiselect'
import {useRouter, useRoute} from 'vue-router';

export default{
    data(){
        return{
        }
    },
    components:{
        Multiselect
    },
    emits:[
        "update:FiltersData"
    ],
    computed:{
        filters:{
            get(){
                return this.FiltersData
            },
            set(value){
                this.$emit('update:FiltersData', value)
            }
        }
    },
    props:{
        FiltersData:[],
        UseQueries:{ 
            type: Boolean, 
            required: false, 
            default: true 
        }
    },
    methods:{
        async applyFilter(){  
            
            if(this.UseQueries){
                console.log(this.$route.query)
                var quries = JSON.parse(JSON.stringify(this.$route.query));

                for (let index = 0; index < this.filters.length; index++) {
                    const element = this.filters[index];
                    
                    var value = JSON.parse(JSON.stringify(element.value)) // Получаем значение из прокси

                    if(value != null){
                        quries[element.filterName] = value
                    }
                    else if (quries[element.filterName] != null){
                    delete quries[element.filterName]
                    }
                }

                this.$router.push({path: this.$route.fullPath, query: quries, params: this.$route.params });
            }

            this.$emit("applyFilter")
        },
        handleCheckboxChange(obj) {
            if (obj.value === null) {
                obj.value = true;
                return
            } else if (obj.value === true) {
                obj.value = false;
                return
            } else if (obj.value === false) {
                obj.value = null;
                return
            }
        }
    },
    async mounted(){
        if(this.UseQueries){
            await this.$router.isReady()
        
            for (let index = 0; index < this.filters.length; index++) {
                const element = this.filters[index];
                
                if(this.$route.query[element.filterName] != null ){
                    element.value = this.$route.query[element.filterName]

                    if(element.type === 'checkbox' && (typeof element.value === 'string' || element.value instanceof String)){
                        element.value = element.value == 'true'
                    }

                    if(element.type === 'multi-select' && !(typeof element.value === 'array' || element.value instanceof Array)){
                        element.value = [element.value]
                    }
                }
            }

            this.$emit("applyFilter")
        }
    }
}
</script>

<template>
    <div class="d-flex w-100">
        <hr class="hr me-2" style="width: 5%;"/>
        <span>Фильтры</span>
        <hr class="hr w-100 ms-2" />
    </div>

    <div class="d-flex ">
            <div 
                class="me-3 md-3 d-flex flex-column text-left text-start" 
                v-for="item in this.filters" 
                :style="`width: ${item.weight};`"
            >
                <label 
                    for="" 
                    class="d-flex form-label mb-1 justify-content-start"
                >
                    <p class="me-1 mb-0">{{item.displayName}}</p>
                    
                    <i 
                        class="bi bi-info-circle mb-0" 
                        data-bs-toggle="tooltip" 
                        data-bs-placement="top" 
                        :title="item.discription" 
                    >
                    </i>
                </label>

                <input 
                    class="form-check-input mx-auto"
                    type="checkbox"
                    v-model="item.value"
                    :key="item.value"
                    :indeterminate="item.value == null"
                    @click.prevent="this.handleCheckboxChange(item)"
                    v-if="item.type == 'checkbox'"  
                    />
                <input
                    v-else-if="item.type != 'select' && item.type != 'multi-select'"
                    :type="item.type"
                    class="form-control"
                    name=""
                    id=""
                    v-model="item.value"
                    :placeholder="item.placeholder"
                />
                <Multiselect
                    v-model="item.value"
                    mode="tags"
                    :close-on-select="false"
                    :searchable="true"
                    :create-option="true"
                    :options="item.options"
                     v-else-if ="item.type == 'multi-select'"
                />
                <Multiselect
                    v-model="item.value"
                    :close-on-select="true"
                    :searchable="true"
                    :create-option="true"
                    :options="item.options"
                    v-else
                />      
            </div>
            <button
                    type="button"
                    class="btn btn-primary my-auto justify-content-start"
                    @click="this.applyFilter()"
            >
                Применить
            </button>
    </div>

</template>

<style src="@vueform/multiselect/themes/default.css"></style>
