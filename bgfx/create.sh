#!/bin/bash
lipo -create ./ios-arm64/libbgfxRelease.a ./ios-simulator64/libbgfxRelease.a -output ios/libbgfx.a
lipo -create ./ios-arm64/libbimg_decodeRelease.a ./ios-simulator64/libbimg_decodeRelease.a -output ios/libbimg_decode.a
lipo -create ./ios-arm64/libbimgRelease.a ./ios-simulator64/libbimgRelease.a -output ios/libbimg.a
lipo -create ./ios-arm64/libbxRelease.a ./ios-simulator64/libbxRelease.a -output ios/libbx.a

lipo -info ios/libbgfx.a
lipo -info ios/libbimg_decode.a
lipo -info ios/libbimg.a
lipo -info ios/libbx.a