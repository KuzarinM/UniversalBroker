<script>
export default{
    data(){
        return{
            modalId:"modalId",
            modal:null
        }
    },
    emits:[
        "modalClose"
    ],
    props:{
        modalSize:{ 
            type: String, 
            required: false, 
            default: "md" 
        },
        title:{ 
            type: String, 
            required: false, 
            default: "Загловок" 
        },
        backdrop:{ 
            type: String, 
            required: false, 
            default: "true"  // static для скрытия
        },
    },
    methods:{
        OpenModal(){
           this.modal.show()
        },
        CloseModal(){
            this.modal.hide()
        }
    },
    mounted(){
        this.modal = new bootstrap.Modal(this.$refs.modalWindow)
    }
}
</script>

<template>
<!-- Button trigger modal -->
<button
    type="button"
    class="btn btn-primary btn-lg"
    @click="this.OpenModal()"
>
    Launch
</button>

<!-- Modal -->
<div
    class="modal fade"
    :id="this.modalId"
    tabindex="-1"
    role="dialog"
    aria-labelledby="modalTitleId"
    aria-hidden="true"
    ref="modalWindow"
    :data-bs-backdrop="this.backdrop" 
    data-bs-keyboard="false"
>
    <div 
        :class="`modal-dialog modal-dialog-scrollable modal-${this.modalSize}`" 
        role="document">
        <div class="modal-content">
            <div class="modal-header">
                <slot name="header">
                    <h5 class="modal-title" id="modalTitleId">
                        {{ this.title }}
                    </h5>
                </slot>
                <button
                    type="button"
                    class="btn-close"
                    data-bs-dismiss="modal"
                    aria-label="Close"
                    @click="this.$emit('modalClose')"
                ></button>
            </div>
            <div class="modal-body">
                <slot name="body">
                    <div class="container-fluid">Модальное окно пусто</div>
                </slot>
            </div>
            <div class="modal-footer">
                <slot name="footer">

                </slot>
                <button
                    type="button"
                    class="btn btn-secondary"
                    data-bs-dismiss="modal"
                    @click="this.$emit('modalClose')"
                >
                    Закрыть
                </button>
            </div>
        </div>
    </div>
</div>

</template>