#!/bin/bash
# All you had to do was build the damn solution, CJ.
target=${1:-Release} # The engine runs a lot better in Release mode. Use it by default.
xbuild -property:Configuration=$target
