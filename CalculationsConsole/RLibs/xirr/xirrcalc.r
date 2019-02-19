xirr <- function(cf, t) {
    npv <- function(cf, t, r) sum(cf / ((r) ^ (t / 365.25)))
    tryCatch(
      irr <- uniroot(f = npv, interval = c(0, 100), cf = cf, t = t, maxiter = 100)$root - 1,
      error = return(0)
    )
    return(irr)
}