#!/bin/bash
git submodule deinit --all -f && git submodule init && git submodule 
sync && git submodule update
