package com.siadminmovie.repository;

import org.springframework.data.jpa.repository.JpaRepository;

import com.siadminmovie.model.Movie;

public interface MovieRepository extends JpaRepository<Movie, Integer> {
    
}
