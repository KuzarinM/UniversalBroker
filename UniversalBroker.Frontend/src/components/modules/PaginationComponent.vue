<script>
export default{
    data(){
        return{
            _pageSizeOptions:[
                1,
                10,
                25,
                50,
                100
            ],
            _windowSize:2,
            _goToPage:null,
        }
    },
    computed:{
        _totalPages:{
            get(){
                return this.TotalPages
            },
            set(value) {
                this.$emit('update:TotalPages', value)
            }
        },
        currentPage:{
            get(){
                return this.PageNumber
            },
            set(value) {
                console.log(value)
                this.$emit('update:PageNumber', value)
            }
        },
        pageSize:{
            get(){
                return this.PageSize
            },
            set(value) {
                this.$emit('update:PageSize', value)
            }
        },
        windowStartPoint(){
            return Math.max(this.currentPage - this._windowSize - 1, 1)
        },
        windowEndPoint(){
            return Math.min(this.currentPage + this._windowSize, this._totalPages-1)
        }
    },
    props:{
        TotalPages:Number,
        PageNumber:Number,
        PageSize:Number,
        UseQueries:{ 
            type: Boolean, 
            required: false, 
            default: true 
        }
    },
    emits: [
        'update:TotalPages', 
        'update:PageNumber',
        'update:PageSize'
    ],
    watch: 
    {
        TotalPages(oldTotalPages, newTotalPages){
            this.SaveParams(); // Как только значение в родителе обновилось, нужно обновить его в пути
        },
        PageNumber(oldPageNumber, newPageNumber){
            this.SaveParams(); // Как только значение в родителе обновилось, нужно обновить его в пути
        },
        PageSize(oldPageSize, newPageSize){
            this.SaveParams(); // Как только значение в родителе обновилось, нужно обновить его в пути
        }
    },
    methods:{
        MoveToPage(page){
            if(page>0 && page <= this._totalPages){
                this.currentPage = page
            }
            this._goToPage=null
        },
        SaveParams(){

            if(!this.UseQueries)
            return

            var quries = JSON.parse(JSON.stringify(this.$route.query));

            var params = {
                pageSize:this.pageSize,
                pageNumber:this.currentPage
            }

            for (const [key, value] of Object.entries(params)) {
                if(value != null){
                    quries[key] = value
                }
                else if (quries[key] != null){
                    delete quries[key]
                }
            }

            console.log(quries)
            this.$router.push({path: this.$route.fullPath, query: quries, params: this.$route.params });
        }
    },
    async mounted(){
        if(this.UseQueries){
            await this.$router.isReady()

            if(this.$route.query.pageSize != null)
                this.pageSize = parseInt(this.$route.query.pageSize)

            if(this.$route.query.pageNumber != null)
                this.currentPage = parseInt(this.$route.query.pageNumber)
        }
    }
}
</script>

<template>

   <nav 
    aria-label="Page navigation" 
    class="d-flex justify-content-center my-3"
    >
    <ul
        class="pagination my-auto"
    >
        <li 
            :class=" this.currentPage - 1 > 0 ? 'page-item' : 'page-item disabled'" 
            style="height: fit-content;"
        >
            <a class="page-link" aria-label="Previous" @click="this.MoveToPage(this.currentPage-1)">
                <span aria-hidden="true">&laquo;</span>
            </a>
        </li>

        <li 
            :class="this.currentPage == 1? 'page-item active' : 'page-item'" aria-current="page"
            style="height: fit-content;"
        >
            <a class="page-link" @click="this.MoveToPage(1)">1</a>
        </li>

        <li 
            class="page-item disabled" 
            aria-current="page" 
            v-if="this.windowStartPoint > 1"
            style="height: fit-content;"
        >
            <p class="page-link my-0">...</p>
        </li>

        <li 
            :class="(index + this.windowStartPoint) == this.currentPage? 'page-item active' : 'page-item'" 
            v-for="index in Math.max(this.windowEndPoint - this.windowStartPoint, 0)"
            style="height: fit-content;"
        >
            <a class="page-link" @click="this.MoveToPage(index + this.windowStartPoint)">{{index + this.windowStartPoint}}</a>
        </li>

        <li 
            class="page-item disabled" 
            aria-current="page" 
            v-if="this.windowEndPoint < this._totalPages-1"
            style="height: fit-content;"
        >
            <p class="page-link my-0">...</p>
        </li>


        <li 
            :class="this.currentPage == this._totalPages? 'page-item active' : 'page-item'"
            style="height: fit-content;"
            v-if="this._totalPages>1"
        >
            <a class="page-link" @click="this.MoveToPage(this._totalPages)">{{this._totalPages}}</a>
        </li>

        <li 
            :class=" this.currentPage + 1 <= this._totalPages ? 'page-item' : 'page-item disabled'"
            style="height: fit-content;"
        >
            <a class="page-link" aria-label="Next" @click="this.MoveToPage(this.currentPage+1)">
                <span aria-hidden="true">&raquo;</span>
            </a>
        </li>
    </ul>
    <select
        class="form-select form-select-sm ms-3 me-2 my-auto"
        name=""
        id=""
        style="width: fit-content; height: fit-content;"
        v-model="this.pageSize"
    >
        <option v-for="item in this._pageSizeOptions" :value="item">по {{item}}</option>
    </select>
    <div class="d-flex ">
        <p class="my-auto me-1">На страницу</p>
        <input
                v-model="this._goToPage"
                type="number"
                class="form-control form-control-sm me-1 my-auto"
                id=""
                min="1"
                :max="this._totalPages"
                style="width: fit-content; height: fit-content; width: 100px;"
                @change="this.MoveToPage(this._goToPage)"
            />
    </div>
   </nav>
   
    
</template>