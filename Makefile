SHELL := /bin/bash
.SILENT:
	
services:
	docker-compose -f docker-compose-services.yml up