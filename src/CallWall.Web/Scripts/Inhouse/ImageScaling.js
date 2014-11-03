function ScaleImage(srcwidth, srcheight, targetwidth, targetheight, fLetterBox) {
    var result = { width: 0, height: 0, fScaleToTargetWidth: true };

    if ((srcwidth <= 0) || (srcheight <= 0) || (targetwidth <= 0) || (targetheight <= 0)) {
        return result;
    }

    // scale to the target width
    var scaleX1 = targetwidth;
    var scaleY1 = (srcheight * targetwidth) / srcwidth;

    // scale to the target height
    var scaleX2 = (srcwidth * targetheight) / srcheight;
    var scaleY2 = targetheight;

    // now figure out which one we should use
    var fScaleOnWidth = (scaleX2 > targetwidth);
    if (fScaleOnWidth) {
        fScaleOnWidth = fLetterBox;
    }
    else {
        fScaleOnWidth = !fLetterBox;
    }

    if (fScaleOnWidth) {
        result.width = Math.floor(scaleX1);
        result.height = Math.floor(scaleY1);
        result.fScaleToTargetWidth = true;
    }
    else {
        result.width = Math.floor(scaleX2);
        result.height = Math.floor(scaleY2);
        result.fScaleToTargetWidth = false;
    }
    result.targetleft = Math.floor((targetwidth - result.width) / 2);
    result.targettop = Math.floor((targetheight - result.height) / 2);

    return result;
}
function OnImageLoad(evt) {
    console.log('OnImageLoad(%O)', evt);

    var img = evt.currentTarget;

    //Take a css free in-memory copy of image to get real/untainted dimensions
    //http://stackoverflow.com/questions/318630/get-real-image-width-and-height-with-javascript-in-safari-chrome
    $("<img/>") 
        .attr("src", $(img).attr("src"))
        .load(function () {
            
            //This is potentially overkill as naturalWidth / naturalHeight should return the same value

            // what's the size of this image and it's parent
            var w = this.width;     // Note: $(this).width() will not
            var h = this.height;    // work for in memory images.
            var tw = $(img).parent().width();
            var th = $(img).parent().height();

            // compute the new size and offsets
            console.log('OnImageLoad - ScaleImage(%i, %i, %i, %i)', w, h, tw, th);
            var result = ScaleImage(w, h, tw, th, false);
            console.log('OnImageLoad - result = %O', result);

            // adjust the image coordinates and size
            img.width = result.width;
            img.height = result.height;
            $(img).css("max-width", result.width);
            $(img).css("max-height", result.height);
            $(img).css("left", result.targetleft);
            $(img).css("top", result.targettop);
        });

    
}