<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, reactive, onMounted, provide} from 'vue';
import { useStore } from 'vuex'
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get } from '@/utils/api';

const store = useStore();
const router = useRouter();

const employees = ref([]);
provide('employees', employees);

const isLoading = ref(true);

onMounted(async () => {
	console.log('Employees main view mounted')

	isLoading.value = true;
	employees.value = await call_get(`/api/employees`);
	isLoading.value = false;
});
</script>
