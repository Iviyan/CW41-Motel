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

const roomTypes = ref([]);
provide('roomTypes', roomTypes);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	roomTypes.value = await call_get(`/api/room-types`);
	isLoading.value = false;
});
</script>
