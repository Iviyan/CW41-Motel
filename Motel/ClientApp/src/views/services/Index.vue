<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, onMounted, provide} from 'vue';
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get } from '@/utils/api';

const router = useRouter();

const services = ref([]);
provide('services', services);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	services.value = await call_get(`/api/services`)
	services.value.sort((a,b) => (a.isActual === b.isActual) ? 0 : a.isActual ? -1 : 1);
	console.log(services.value);
	isLoading.value = false;
});
</script>
